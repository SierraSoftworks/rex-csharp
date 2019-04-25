import * as React from 'react';
import { Route, Redirect } from 'react-router';
import Layout from './components/Layout';
import Home from './components/Home';
import Ideas from './components/Ideas';

export default () => (
    <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/ideas' component={Ideas} />
        <Route path='/login' component={Redirect} to="/" />
    </Layout>
);
