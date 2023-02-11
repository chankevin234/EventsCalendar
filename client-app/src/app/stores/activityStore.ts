// this uses MOBX Store
import { makeAutoObservable, reaction, runInAction } from "mobx";
import agent from "../api/agent";
import { Activity, ActivityFormValues } from "../models/activity";
import { format } from "date-fns";
import { store } from "./store";
import { Profile } from "../models/profile";
import { Pagination, PagingParams } from "../models/pagination";

export default class ActivityStore {
    activityRegistry = new Map<string, Activity>(); // acitivties should be sorted by date
    selectedActivity: Activity | undefined = undefined; 
    editMode = false;
    loading = false;
    loadingInitial = false;
    pagination: Pagination | null = null;
    pagingParams = new PagingParams();
    predicate = new Map().set('all', true);

    constructor() {
        makeAutoObservable(this) //auto recognizes the functions and properties in the class and makes them observable

        reaction(
            () => this.predicate.keys(),
            () => {
                this.pagingParams = new PagingParams();
                this.activityRegistry.clear();
                this.loadActivities();
            }
        )
    }

    //set the paging parameters
    setPagingParams = (pagingParams: PagingParams) => {
        this.pagingParams = pagingParams;
    }

    setPredicate = (predicate: string, value: string | Date) => {
        const resetPredicate = () => {
            this.predicate.forEach((value, key) => {
                if (key !== 'startDate') {
                    this.predicate.delete(key);
                }
            })
        }
        switch (predicate) {
            case 'all':
                resetPredicate();
                this.predicate.set('all', true);
                break;
            case 'isGoing':
                resetPredicate();
                this.predicate.set('isGoing', true);
                break;
            case 'isHost':
                resetPredicate();
                this.predicate.set('isHost', true);
                break;
            case 'startDate':
                this.predicate.delete('startDate');
                this.predicate.set('startDate', value);
        }
    }

    get axiosParams() {
        const params = new URLSearchParams();
        params.append('pageNumber', this.pagingParams.pageNumber.toString());
        params.append('pageSize', this.pagingParams.pageSize.toString());
        this.predicate.forEach((value, key) => {
            if (key === 'startDate') {
                params.append(key, (value as Date).toISOString())
            } else {
                params.append(key,value);
            }
        })
        return params;
    }

    get activitiesByDate() { //this 
        return Array.from(this.activityRegistry.values()).sort((a, b) => 
            a.date!.getTime() - b.date!.getTime());
    }

    get groupedActivities() {
        return Object.entries(
            this.activitiesByDate.reduce((activities, activity) => {
                const date = format(activity.date!, 'dd MMM yyyy'); // the key for each act obj
                // checks if the activity obj at the date has a match in the MAP
                activities[date] = activities[date] ? [...activities[date], activity] : [activity];
                return activities; //returns array of activities matching this date 
            }, {} as {[key: string]: Activity[]})
        )
    }

    //loads ALL the activities in your registry
    loadActivities = async () => {
        this.setLoadingInitial(true); // changes the property 
        try {
            const result = await agent.Activities.list(this.axiosParams);         
            result.data.forEach(activity => {
                this.setActivity(activity); // SET
            })
            this.setPagination(result.pagination);
            this.setLoadingInitial(false); // changes the property 
        } catch (error) {
            console.log(error)
            this.setLoadingInitial(false);; // changes the property
        }
    }

    //set the pagination
    setPagination = (pagination: Pagination) => {
        this.pagination = pagination;
    }

    // loads 1 activity in your registry
    loadActivity = async (id: string) => {
        let activity = this.getActivity(id);
        if (activity) {
            this.selectedActivity = activity; // set selected act as your chosen act (if the act exists in the registry otherwise, check API)
            return activity;
        }
        else {
            this.setLoadingInitial(true);
            try {
                // this is grabbing info from the API 
                activity = await agent.Activities.details(id);
                this.setActivity(activity);
                runInAction(() => this.selectedActivity = activity); 
                this.setLoadingInitial(false);
                return activity;
            } catch (error) {
                console.log(error);
                this.setLoadingInitial(false); // this occurs if the value of activity is undefined
            }
        }
        
    }

    //this is a private function (only used in this class)
    private getActivity = (id: string) => { 
        // gets the activity in the registry map 
        return this.activityRegistry.get(id);
    }
    //this is a private function (only used in this class)
    private setActivity = (activity: Activity) => {
        const user = store.userStore.user; //get 1 user

        if (user) {
            activity.isGoing = activity.attendees!.some(
                //some returns a bool
                a => a.username === user.username
            )
            activity.isHost = activity.hostUsername === user.username //bool
            activity.host = activity.attendees?.find(x => x.username === activity.hostUsername);
        }
        activity.date = new Date(activity.date!);
        this.activityRegistry.set(activity.id, activity); // pushing into this MAP obj in class and update state
    }


    setLoadingInitial = (state: boolean) => { //occurs in its own action
        this.loadingInitial = state;
    }

    // SINCE we are using routing, no need for individual functions 
    // -----------------------------------------------------------------------------------------------
    // selectActivity = (id: string) => { // looks for activity in the activities array
    //     this.selectedActivity = this.activityRegistry.get(id); //retruns Activity obj at w/ id key
    // }

    // cancelSelectedActivity = () => { // sets the selected activity as undefined to cancel
    //     this.selectedActivity = undefined;
    // }

    // openForm = (id?: string) => {
    //     // optional id means that this function can be used for creating act or editing act
    //     // does this id exist? if not, cancel
    //     id ? this.selectActivity(id) : this.cancelSelectedActivity();
    //     this.editMode = true; //sets the state as opened
    // }

    // closeForm = () => {
    //     this.editMode = false; //changes the edit mode back to false
    // }

    createActivity = async (activity: ActivityFormValues) => { //create a new activity
        // also add a host when creating a new act
        const user = store.userStore.user;
        const attendee = new Profile(user!);

        try {
            await agent.Activities.create(activity);
            const newActivity = new Activity(activity);
            newActivity.hostUsername = user!.username;
            newActivity.attendees = [attendee];
            this.setActivity(newActivity);

            runInAction(() => {
                this.selectedActivity = newActivity;
                
            })
        } catch (error) {
            console.log(error);
        
        }
    }

    updateActivity = async (activity: ActivityFormValues) => { //update existing activity
        try {
            await agent.Activities.update(activity);
            runInAction(() => {
                if (activity.id) {
                    let updatedActivity = {
                        ...this.getActivity(activity.id),
                        ...activity
                    }
                    this.activityRegistry.set(activity.id, updatedActivity as Activity);
                    this.selectedActivity = updatedActivity as Activity;
                }          
            })
        } catch (error) {
            console.log(error);
            
        }
    }

    deleteActivity = async (id: string) => { //delete an activity
        this.loading = true;
        try {
            await agent.Activities.delete(id); //delete action from agent.ts
            runInAction(() => {
                this.activityRegistry.delete(id);
                this.loading = false;
            })
        } catch (error) {
            console.log(error);
            runInAction(() => {
                this.loading = false;
            })
        }
    }

    updateAttendance = async () => {
        const user = store.userStore.user;
        this.loading = true;

        try {
            await agent.Activities.attend(this.selectedActivity!.id); // check the response for attending an activity
            runInAction(() => {
                if (this.selectedActivity?.isGoing) { // if "going" is the response, check if they are already there, if so, remove
                    this.selectedActivity.attendees = 
                        this.selectedActivity.attendees?.filter(a => a.username !== user?.username) //removes the attendee obj
                    this.selectedActivity.isGoing = false;
                } else { //else add them to the activity
                    const attendee = new Profile(user!);
                    this.selectedActivity?.attendees?.push(attendee);
                    this.selectedActivity!.isGoing = true;
                }
                this.activityRegistry.set(this.selectedActivity!.id, this.selectedActivity!);

            })
        } catch (error) {
            console.log(error);           
        } finally {
            runInAction(() => this.loading = false);
        }
    }

    cancelActivityToggle = async () => {
        this.loading = true;
        try {
            await agent.Activities.attend(this.selectedActivity!.id);
            runInAction(() => {
                this.selectedActivity!.isCancelled = !this.selectedActivity?.isCancelled;
                this.activityRegistry.set(this.selectedActivity!.id, this.selectedActivity!);
            })
        } catch (error) {
            console.log(error);
            
        } finally {
            runInAction(() => this.loading = false);
        }
    }

    clearSelectedActivity = () => {
        this.selectedActivity = undefined;
    }

    updateAttendeeFollowing = (username: string) => {
        this.activityRegistry.forEach(activity => {
            activity.attendees?.forEach(attendee => {
                if (attendee.username === username) {
                    attendee.following ? attendee.followersCount-- : attendee.followersCount++;
                    attendee.following = !attendee.following;
                }
            })
        })
    }
}