import { navigate } from "svelte-routing";
import { writable } from 'svelte/store';

export const isAuthenticated = writable(false); // Store for login status
export const userName = writable(''); // Store for username

function decodeJwt(token) {
    if (!token) {
        console.warn("No token provided. Redirecting to login.");
        navigate("/login");
        return null;
    }
    try {
        const payloadBase64 = token.split('.')[1]; // Extract the payload part of the JWT
        const decodedPayload = JSON.parse(atob(payloadBase64)); // Decode the base64 string and parse as JSON
        return decodedPayload;
    } catch (error) {
        console.error("Failed to decode token:", error);
        navigate("/login");
        return null;
    }
}

function getRoleFromToken(decodedToken) {
    if (!decodedToken) {
        console.warn("Decoded token is missing. Cannot retrieve role.");
        return null;
    }
    return decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
}


function isAdmin(decodedToken) {
    if (!decodedToken) {
        alert("User is not logged in.");
        return false;
    }

    const role = getRoleFromToken(decodedToken);

    if (typeof role === "string") {
        return role === "Admin";
    }

    if (Array.isArray(role)) {
        return role.includes("Admin");
    }

    return false;
}

function isFromUser(userId, decodedToken) {
    if (!decodedToken) {
        console.warn("Decoded token is missing. Cannot verify user ownership.");
        return false;
    }

    const currentUserId = decodedToken.sub;
    return currentUserId === userId;
}

export function getAccessToken() {
    const token = sessionStorage.getItem("accessToken");
    if (!token) {
        console.warn("Access token is missing.");
    }
    else{
        
    }   
    return token;
}

export function checkAuthFunctions(recipeUserId) {
    const token = getAccessToken();
    if (!token) {
        console.warn("User is not authenticated.");
        navigate("/login");
        return false;
    }

    const decodedToken = decodeJwt(token);
    if (!decodedToken) {
        return false;
    }

    if (isAdmin(decodedToken)) {
        console.log("Current user is admin. Provide edit functions.");
        return true;
    } else if (isFromUser(recipeUserId, decodedToken)) {
        console.log("This recipe belongs to the current user. Provide edit functions.");
        return true;
    }

    console.log("User is neither admin nor the owner of the recipe.");
    return false;
}

export function getUserName() {
    const token = getAccessToken();
    if (!token) {
        console.warn("Cannot retrieve username. User is not authenticated.");
        return null;
    }

    const decodedToken = decodeJwt(token);
    if (!decodedToken) {
        return null;
    }

    const name = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];
    console.log("Retrieved username:", name);
    return name || "Unknown User";
}

export function isLoggedIn() {
    const token = getAccessToken();
    if (!token) {
        console.log("User is not logged in.");
        return false;
    }
    else{
        isAuthenticated.set(true);
        userName.set(getUserName());
    }
    return true;
}