import * as React from 'react';
import { connect } from 'react-redux';
import { RouteComponentProps } from 'react-router';
import { Link } from 'react-router-dom';
import { ApplicationState } from '../store';
import * as IdeasStore from '../store/Ideas';

// At runtime, Redux will merge together...
type IdeasProps =
    IdeasStore.IdeasState // ... state we've requested from the Redux store
    & typeof IdeasStore.actionCreators // ... plus action creators we've requested
    & RouteComponentProps<{ tag: string, complete: string }>; // ... plus incoming routing parameters


class Ideas extends React.PureComponent<IdeasProps> {
    // This method is called when the component is first added to the document
    public componentDidMount() {
        this.ensureDataFetched();
    }

    // This method is called when the route parameters change
    public componentDidUpdate() {
        this.ensureDataFetched();
    }

    public render() {
        return (
            <React.Fragment>
                <h1>Ideas</h1>
                {this.renderIdeasTable()}
                {this.renderPagination()}
            </React.Fragment>
        );
    }

    private ensureDataFetched() {
        const startDateIndex = 0;
        this.props.requestIdeas(startDateIndex);
    }

    private renderIdeasTable() {
        return (
            <table className='table table-striped'>
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Description</th>
                        <th>Completed</th>
                        <th>Tags</th>
                    </tr>
                </thead>
                <tbody>
                    {this.props.ideas.map((idea: IdeasStore.Idea) =>
                        <tr key={idea.id}>
                            <td>{idea.name}</td>
                            <td>{idea.description}</td>
                            <td>Completed: {idea.completed}</td>
                            <td>Tags: {idea.tags.join(", ")}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    private renderPagination() {
        const prevStartDateIndex = (this.props.startDateIndex || 0) - 5;
        const nextStartDateIndex = (this.props.startDateIndex || 0) + 5;

        return (
            <div className="d-flex justify-content-between">
                <Link className='btn btn-outline-secondary btn-sm' to={`/ideas/${prevStartDateIndex}`}>Previous</Link>
                {this.props.isLoading && <span>Loading...</span>}
                <Link className='btn btn-outline-secondary btn-sm' to={`/ideas/${nextStartDateIndex}`}>Next</Link>
            </div>
        );
    }
}

export default connect(
    (state: ApplicationState) => state.ideas, // Selects which state properties are merged into the component's props
    IdeasStore.actionCreators // Selects which action creators are merged into the component's props
)(Ideas as any);
