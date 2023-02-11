import { SyntheticEvent, useEffect } from 'react';
import { observer } from 'mobx-react-lite';
import { Tab, Grid, Header, Card, Image, TabProps, CardContent } from 'semantic-ui-react';
import { Link } from 'react-router-dom';
import { UserActivity } from '../../app/models/profile';
import { format } from 'date-fns';
import { useStore } from "../../app/stores/store";

/**
 * each profile activity is contained in its own card. This component should have 3 tabs to allow the user to select from:
        1. Future activities
        2. Past activities
        3. Activities the user is hosting
 */

const panes = [
    {menuItem: 'Future Activities', pane: {key: 'future'}},
    {menuItem: 'Past Events', pane: {key: 'past'}},
    {menuItem: 'Hosting', pane: {key: 'hosting'}}
]

export default observer(function ProfileActivities() {
    const {profileStore} = useStore();
    const {loadUserActivities, profile, loadingActivities, userActivities} = profileStore;
    
    useEffect(() => {
        loadUserActivities(profile!.username);
    }, [loadUserActivities, profile]);
    
    const handleTabChange = (e: SyntheticEvent, data: TabProps) => {
        loadUserActivities(profile!.username, panes[data.activeIndex as number].pane.key);
    };

    return (
        <Tab.Pane loading={loadingActivities}>
            <Grid>
                <Grid.Column width={16}>
                    <Header floated="left" icon='calendar' content={'Activities'} />
                </Grid.Column>
                <Grid.Column width={16}>
                    <Tab 
                        panes={panes}
                        menu={{secondary: true, pointing: true}}
                        onTabChange={(e, data) => handleTabChange(e, data)}
                    />
                    <br />
                    <Card.Group itemsPerRow={4}>
                        {userActivities.map((activity: UserActivity) => (
                            <Card 
                                as={Link}
                                to={`/activities/${activity.id}`}
                                key={activity.id}
                            >
                                <Image 
                                    src={`/assets/categoryImages/${activity.category}.jpg`}
                                    style={{minHeight: 100, objectFit: 'cover'}}
                            
                                />
                                <CardContent>
                                    <Card.Header textAlign="center">
                                        {activity.title}
                                    </Card.Header>
                                    <Card.Meta textAlign="center">
                                        <div>{format(new Date(activity.date), 'do LLL')}</div>
                                        <div>{format(new Date(activity.date), 'h:mm a')}</div>
                                    </Card.Meta>
                                </CardContent>
                            </Card>
                        ))}
                    </Card.Group>
                </Grid.Column>
            </Grid>
        </Tab.Pane>
    );
})