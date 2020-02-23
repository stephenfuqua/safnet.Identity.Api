import React from 'react';
import { connect } from 'react-redux';

const Home = props => (
    <div>
        <p>Hello world</p>
    </div>
);

export default connect()(Home);

