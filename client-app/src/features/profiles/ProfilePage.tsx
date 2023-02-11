import { observer } from "mobx-react-lite";
import { Fragment, useEffect } from "react";
import { useParams } from "react-router-dom";
import { Grid } from "semantic-ui-react";
import LoadingComponent from "../../app/layout/LoadingComponent";
import { useStore } from "../../app/stores/store";
import ProfileContent from "./ProfileContent";
import ProfileHeader from "./ProfileHeader";

export default observer(function ProfilePage() {
    const { username } = useParams<{ username: string }>(); //using a react router hook to go and get the paramas from root 
    const { profileStore } = useStore();
    const { loadingProfile, loadProfile, profile, setActiveTab } = profileStore;

    useEffect(() => { //used to call the loadProfile method
        if (username) {
            loadProfile(username);
            return () => {
                setActiveTab(0);
            }
        }
    }, [loadProfile, username, setActiveTab])

    if (loadingProfile) {
        return <LoadingComponent content="Loading profile..." />
    }

    return (
        <Grid>
            <Grid.Column width={16}>
                {profile &&
                    <Fragment>
                        <ProfileHeader profile={profile} />
                        <ProfileContent profile={profile} />
                    </Fragment>}
            </Grid.Column>
        </Grid>
    )
})