import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useStore } from "../stores/store";

export default function RequireAuth() {
    const {userStore: {isLoggedIn}} = useStore();
    const location = useLocation();

    //check if the user is logged in
    if (!isLoggedIn) {
        return <Navigate to='/' state={{from: location}} /> //navigate back to homepage
    }

    return <Outlet />
}