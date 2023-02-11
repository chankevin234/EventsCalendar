// this uses MOBX Store

import { makeAutoObservable, reaction } from "mobx";
import { ServerError } from "../models/serverError";

export default class CommonStore {
    error: ServerError | null = null;
    token: string | null = localStorage.getItem('jwt');
    appLoaded = false;

    constructor() {
        makeAutoObservable(this);

        //use a mobx REACTION() to observe the changes in the token prop's state
        reaction( //doesn't occur when token is initially set, but runs after 'token' changes
            () => this.token, //react to the token prop
            token => { //check if token exists
                if (token) {
                    localStorage.setItem('jwt', token); //set
                }
                else {
                    localStorage.removeItem('jwt'); // remove/logout
                }
            }
        ) 
    }

    setServerError(error: ServerError) {
        this.error = error;
    }

    setToken = (token: string | null) => { //sets whether the user has authenticated once
        this.token = token;
    }

    setAppLoaded = () => { // checks that the user has already be logged in
        this.appLoaded = true;
    }
}