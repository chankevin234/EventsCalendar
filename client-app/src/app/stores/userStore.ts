// this uses MOBX Store
import { makeAutoObservable, runInAction } from "mobx";
import agent from "../api/agent";
import { User, UserFormValues } from "../models/user";
import { router } from "../router/Routes";
import { store } from "./store";

export default class UserStore {
    user: User | null = null; //determines if user is actually logged in or 'null'

    constructor() {
        makeAutoObservable(this)
    }

    get isLoggedIn() {
        return !!this.user // !! = converts non-bool to boolean
    }

    login = async (creds:UserFormValues) => {
        try {
            const user = await agent.Account.login(creds)
            store.commonStore.setToken(user.token); // using the setToken method from 'commonStore.ts'
            runInAction(() => this.user = user) // MOBX function to set the "User property" in this class
            router.navigate('/activities');
            store.modalStore.closeModal();//close modal
            console.log(user); //user info on console
        } catch (error) {
            throw error; // throw an error to the "onSubmit hook" in LoginForm.tsx
        }
    }

    register = async (creds:UserFormValues) => {
        try {
            const user = await agent.Account.register(creds)
            store.commonStore.setToken(user.token); // using the setToken method from 'commonStore.ts'
            runInAction(() => this.user = user) // MOBX function to set the "User property" in the store
            router.navigate('/activities');
            store.modalStore.closeModal();//close modal after registering
            console.log(user); //user info on console
        } catch (error) {
            throw error; // throw an error to the "onSubmit hook" in LoginForm.tsx
        }
    }

    logout = () => {
        store.commonStore.setToken(null); //removes token
        // REACTION method from 'commonStore.ts' replaces the localstorage.removeItem method 
        this.user = null;
        router.navigate('/'); //back to homepage
    }

    getUser = async () => {
        try {
            const user = await agent.Account.current(); //get the currently selected user
            runInAction(() => this.user = user); //sets the user
        } catch (error) {
            console.log(error);
        }
    }

    setImage = (image: string) => {
        if (this.user) {
            this.user.image = image;
        }
    }
}  