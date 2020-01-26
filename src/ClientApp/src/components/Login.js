import React from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Login';
import Container from 'react-bootstrap/Container';
import Form from 'react-bootstrap/Form';
import Button from 'react-bootstrap/Button';

const Login = props => (
    <Container>
        <h1>Sign-in</h1>

        <Form>
            <Form.Group controlId="email-address">
                <Form.Label>E-mail Address</Form.Label>
                <Form.Control type="email" placeholder="E-mail address / username" />
            </Form.Group>
            <Form.Group controlId="password">
                <Form.Label>Password</Form.Label>
                <Form.Control type="password" placeholder="Password" />
            </Form.Group>
            <Form.Group controlId="remember-me">
                <Form.Check type="checkbox" label="Remember My Login" />
            </Form.Group>
            <Form.Group>
                <Button variant="primary" type="submit">Login</Button>
            </Form.Group>
        </Form>
    </Container>
);

export default connect(
    state => state.counter,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Login);
