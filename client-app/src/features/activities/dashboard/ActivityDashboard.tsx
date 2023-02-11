import { Fragment, useEffect, useState } from "react";
import { Grid, Loader } from "semantic-ui-react";
import ActivityList from "./ActivityList";
import { useStore } from "../../../app/stores/store";
import { observer } from "mobx-react-lite";
import ActivityFilters from "./ActivityFilters";
import { PagingParams } from "../../../app/models/pagination";
import InfiniteScroll from "react-infinite-scroller";
import ActivityListItemPlaceholder from "./ActivityListItemPlaceholder";

export default observer(function ActivityDashboard() {
    // monitors changes in state!
    const { activityStore } = useStore();
    const { loadActivities, activityRegistry, setPagingParams, pagination } = activityStore;
    const [loadingNext, setLoadingNext] = useState(false);

    function handleGetNext() {
        setLoadingNext(true);
        setPagingParams(new PagingParams(pagination!.currentPage + 1));
        loadActivities().then(() => setLoadingNext(false));
    }

    useEffect(() => {
        if (activityRegistry.size <= 1) loadActivities(); // if there is stuff in the actReg, don't need to load from the api
    }, [loadActivities, activityRegistry.size]) // 2 dependency in square brackets

    //check if the screen is in "loading" state 
    // ------(NOT NEEDED ANYMORE SINCE USING PLACEHOLDER-SemanticsUI)
    // if (activityStore.loadingInitial && !loadingNext) return <LoadingComponent content='Loading Activities' />
    return (
        <Grid>
            <Grid.Column width={'10'}>
                {activityStore.loadingInitial && !loadingNext ? (
                    <Fragment>
                        <ActivityListItemPlaceholder />
                        <ActivityListItemPlaceholder />
                    </Fragment>
                ) : (
                    <InfiniteScroll
                        pageStart={0}
                        loadMore={handleGetNext}
                        hasMore={!loadingNext && !!pagination && pagination.currentPage < pagination.totalPages}
                        initialLoad={false}
                    >
                        <ActivityList />
                    </InfiniteScroll>
                )}
            </Grid.Column>
            <Grid.Column width='6'>
                <ActivityFilters />
            </Grid.Column>
            <Grid.Column width={10}>
                <Loader active={loadingNext} />
            </Grid.Column>
        </Grid>
    )
})