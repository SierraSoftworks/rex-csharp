import { Action, Reducer } from 'redux';
import { AppThunkAction } from './';
import { runWithAuth, getToken } from "../auth";

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface IdeasState {
    isLoading: boolean;
    startDateIndex?: number;
    ideas: Idea[];
}

export interface Idea {
    id: string;
    name: string;
    description: string;
    tags: string[];
    completed: boolean;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestIdeasAction {
    type: 'REQUEST_IDEAS';
    startDateIndex: number;
}

interface ReceiveIdeasAction {
    type: 'RECEIVE_IDEAS';
    startDateIndex: number;
    ideas: Idea[];
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = RequestIdeasAction | ReceiveIdeasAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
    requestIdeas: (startDateIndex: number): AppThunkAction<KnownAction> => (dispatch, getState) => {
        // Only load data if it's something we don't already have (and are not already loading)
        const appState = getState();
        if (appState && appState.ideas && startDateIndex !== appState.ideas.startDateIndex) {
            fetch(`/api/v2/ideas`, {
                headers: {
                    "Authorization": `Bearer ${getToken()}`,
                    "Accept": "application/json"
                }
            }).then(response => response.json() as Promise<Idea[]>)
                .then(data => {
                    dispatch({ type: 'RECEIVE_IDEAS', startDateIndex: startDateIndex, ideas: data });
                });

            dispatch({ type: 'REQUEST_IDEAS', startDateIndex: startDateIndex });
        }
    }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: IdeasState = { ideas: [], isLoading: false };

export const reducer: Reducer<IdeasState> = (state: IdeasState | undefined, incomingAction: Action): IdeasState => {
    if (state === undefined) {
        return unloadedState;
    }

    const action = incomingAction as KnownAction;
    switch (action.type) {
        case 'REQUEST_IDEAS':
            return {
                startDateIndex: action.startDateIndex,
                ideas: state.ideas,
                isLoading: true
            };
        case 'RECEIVE_IDEAS':
            // Only accept the incoming data if it matches the most recent request. This ensures we correctly
            // handle out-of-order responses.
            if (action.startDateIndex === state.startDateIndex) {
                return {
                    startDateIndex: action.startDateIndex,
                    ideas: action.ideas,
                    isLoading: false
                };
            }
            break;
    }

    return state;
};
