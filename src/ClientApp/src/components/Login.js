import React from 'react';
import { Formik, Field, ErrorMessage } from 'formik';
import Container from 'react-bootstrap/Container';
import Form from 'react-bootstrap/Form';
import FormGroup from 'react-bootstrap/FormGroup'
import FormLabel from 'react-bootstrap/FormLabel'
import FormControl from 'react-bootstrap/FormControl'
import FormCheck from 'react-bootstrap/FormCheck'
import Button from 'react-bootstrap/Button';

class Login extends React.Component {
    render() {
        return (
            <Container>
                <h1>Sign-in</h1>
                <Formik
                    initialValues={{ emailAddress: '', password: '', rememberMe: false }}
                    onSubmit={values => {
                        alert(JSON.stringify(values, null, 2));
                    }}
                >
                    {({
                        values,
                        errors,
                        touched,
                        handleChange,
                        handleBlur,
                        handleSubmit,
                        isSubmitting
                    }) => (
                            <Form onSubmit={handleSubmit}>
                                <FormGroup controlId="emailAddress">
                                    <FormLabel>Email Address</FormLabel>
                                    <FormControl
                                        name="emailAddress"
                                        type="email"
                                        onChange={handleChange}
                                        value={values.emailAddress}
                                    />
                                </FormGroup>
                                <FormGroup controlId="password">
                                    <FormLabel>Password</FormLabel>
                                    <FormControl
                                        placeholder="Password"
                                        name="password"
                                        type="password"
                                        onChange={handleChange}
                                        value={values.password}
                                    />
                                </FormGroup>
                                <FormGroup controlId="rememberMe">
                                    <FormCheck
                                        label="Remember My Login"
                                        name="rememberMe"
                                        onChange={handleChange}
                                        checked={values.rememberMe} />
                                </FormGroup>
                                <Button type="submit"
                                    variant="primary">
                                    Submit
                                </Button>
                            </Form>
                        )}
                </Formik>
            </Container>
        );
    }
}

export default Login;