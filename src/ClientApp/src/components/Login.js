import React from 'react';
import { Formik, ErrorMessage } from 'formik';
import Container from 'react-bootstrap/Container';
import Form from 'react-bootstrap/Form';
import FormGroup from 'react-bootstrap/FormGroup'
import FormLabel from 'react-bootstrap/FormLabel'
import FormControl from 'react-bootstrap/FormControl'
import FormCheck from 'react-bootstrap/FormCheck'
import Button from 'react-bootstrap/Button';
import * as Yup from 'yup';
import QueryString from 'query-string';

class Login extends React.Component {
    getReturnUrl() {
        const parsed = QueryString.parse(window.location.search);
        return parsed.returnUrl;
    }

    render() {
        return (
            <Container>
                <h1>Sign-in</h1>
                <Formik
                    initialValues={{ emailAddress: "", password: "", rememberMe: false, returnUrl: this.getReturnUrl() }}
                    onSubmit={values => {
                        alert(JSON.stringify(values, null, 2));
                    }}
                    validationSchema={Yup.object({
                        password: Yup.string()
                            .max(30, 'Must be 30 characters or less')
                            .required('Required'),
                        emailAddress: Yup.string()
                            .email('Invalid email address')
                            .required('Required'),
                    })}
                >
                    {({
                        handleSubmit,
                        getFieldProps,
                        touched,
                        errors
                    }) => (
                            <Form onSubmit={handleSubmit}>
                                <FormControl type="hidden" name="returnUrl" id="returnUrl" {...getFieldProps("returnUrl")} />
                                <FormGroup controlId="emailAddress">
                                    <FormLabel>Email Address</FormLabel>
                                    <FormControl
                                        placeholder="Enter your email address"
                                        name="emailAddress"
                                        type="email"
                                        isInvalid={touched.emailAddress && errors.emailAddress}
                                        isValid={touched.emailAddress && !errors.emailAddress}
                                        {...getFieldProps("emailAddress")}
                                    />
                                    <ErrorMessage name="emailAddress" component="div" className="invalid-feedback d-inline" />
                                </FormGroup>
                                <FormGroup controlId="password">
                                    <FormLabel>Password</FormLabel>
                                    <FormControl
                                        placeholder="Enter your password"
                                        name="password"
                                        type="password"
                                        isInvalid={touched.password && errors.password}
                                        isValid={touched.password && !errors.password}
                                        {...getFieldProps("password")}
                                    />
                                    <ErrorMessage name="password" component="div" className="invalid-feedback d-inline" />
                                </FormGroup>
                                <FormGroup controlId="rememberMe">
                                    <FormCheck
                                        custom
                                        label="Remember My Login"
                                        name="rememberMe"
                                        {...getFieldProps("rememberMe")} />
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