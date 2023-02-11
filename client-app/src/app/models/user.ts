export interface User { 
    // this is an interface for the User object returned by the api
    username: string;
    displayName: string;
    token: string;
    image?: string;
}

export interface UserFormValues {
    // this is an interface for the login and registration forms
    email: string;
    password: string;
    displayName?: string;
    username?: string;
}