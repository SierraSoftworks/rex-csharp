import { AuthenticationContext, AdalConfig, runWithAdal } from "react-adal"

const adalConfig: AdalConfig = {
    tenant: "sierrasft.onmicrosoft.com",
    clientId: "e284d597-f080-406d-a3fc-91f18eca6baa",
    endpoints: {
        api: "https://sierrasft.onmicrosoft.com/e284d597-f080-406d-a3fc-91f18eca6baa"
    },
    postLogoutRedirectUri: window.location.origin,
    redirectUri: window.location.origin + "/login",
    cacheLocation: "sessionStorage"
}

export const authContext = new AuthenticationContext(adalConfig)

export function getToken() {
    return authContext.getCachedToken(authContext.config.clientId)
}

export function getUser() {
    return authContext.getCachedUser()
}

export function runWithAuth<T>(fn: () => void, doNotLogin: boolean = false) {
    runWithAdal(authContext, fn, doNotLogin)
}