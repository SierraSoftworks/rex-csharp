import * as React from 'react';
import { Link } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Button, Avatar } from '@material-ui/core';
import { getUser } from '../auth';

const IdeasLink = (props: any) => <Link to="/ideas" {...props} />
const NewIdeaLink = (props: any) => <Link to="/ideas/new" {...props} />
const RandomIdeaLink = (props: any) => <Link to="/idea/random" {...props} />

export default class NavMenu extends React.PureComponent<{}, {}> {
    public state = {};

    public render() {
        console.log(getUser().profile)
        return (
            <header>
                <AppBar position="sticky">
                    <Toolbar>
                        <Typography variant="h5" noWrap>Randy</Typography>

                        <Button component={NewIdeaLink}>New</Button>
                        <Button component={IdeasLink}>Ideas</Button>
                        <Button component={RandomIdeaLink}>Random</Button>
                    </Toolbar>
                </AppBar>
            </header>
        );
    }
}
