import { Fragment } from 'react';
import { Container } from 'semantic-ui-react';
import NavBar from './NavBar';
import { observer } from 'mobx-react-lite';
import { Outlet, ScrollRestoration, useLocation } from 'react-router-dom';
import HomePage from '../../features/home/HomePage';
import { ToastContainer } from 'react-toastify';
import { useStore } from '../stores/store';
import { useEffect } from 'react';
import LoadingComponent from './LoadingComponent';
import ModalContainer from '../common/modals/ModalContainer';

function App() {
  const location = useLocation();
  const {commonStore, userStore} = useStore();

  //this is a 'side effect' which is a function that runs when this component loads
  useEffect(() => {
    //check if token exists in localstore
    if (commonStore.token) {
      userStore.getUser().finally(() => commonStore.setAppLoaded()); //get the user and turn off loading flag
    }
    else {
      commonStore.setAppLoaded();
    }
  }, [commonStore, userStore]) // 2 dependencies which, when their state changes, cause this hook to run

  // check if the commonStore has a user loaded already
  if (!commonStore.appLoaded) {
    return <LoadingComponent content='Loading app!' />
  }

  return (
    // 1 Element per react component (NavBar and Container = 2 elements at same level)
    /* location.pathname checks whether the address is '/' to show the homepage, 
    otherwise, it shows the navbar and outlet routes */
    <Fragment>
      <ScrollRestoration />
      <ModalContainer />
      <ToastContainer position='bottom-right' theme='colored' />
      {location.pathname === '/' ? <HomePage /> : (
        <Fragment>
          <NavBar />
            <Container style={{marginTop: '7em'}}>
              <Outlet />
            </Container>
        </Fragment>
      )}   
    </Fragment>
  );
}

export default observer(App);
