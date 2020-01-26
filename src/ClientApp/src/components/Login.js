import React from 'react';
import Container from 'react-bootstrap/Container';
import Form from 'react-bootstrap/Form';
import Button from 'react-bootstrap/Button';

class Login extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            emailAddress: '',
            password: '',
            rememberMe: false
        };
    }

    signIn = event => {
        event.preventDefault();
        alert('hello ' + this.state.emailAddress);
    }

    formChangeHandler = event => {
        let nam = event.target.name;
        let val = event.target.value;
        this.setState({ [nam]: val });
    }

    render() {
        return <Container>
            <h1>Sign-in</h1>

            <Form>
                <Form.Group controlId="email-address">
                    <Form.Label>E-mail Address</Form.Label>
                    <Form.Control type="email" placeholder="E-mail address / username" name="emailAddress" onChange={this.formChangeHandler} required />
                </Form.Group>
                <Form.Group controlId="password">
                    <Form.Label>Password</Form.Label>
                    <Form.Control type="password" placeholder="Password" name="password" onChange={this.formChangeHandler} required />
                </Form.Group>
                <Form.Group controlId="remember-me">
                    <Form.Check type="checkbox" label="Remember My Login" name="rememberMe" onChange={this.formChangeHandler} />
                </Form.Group>
                <Form.Group>
                    <Button variant="primary" type="submit" onSubmit={this.signIn} onClick={this.signIn}>Login</Button>
                </Form.Group>
            </Form>
        </Container>
    }
}

export default Login;