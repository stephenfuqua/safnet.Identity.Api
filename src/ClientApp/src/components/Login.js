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
import Axios from 'axios';

class Login extends React.Component {
    getReturnUrl() {
        const parsed = QueryString.parse(window.location.search);
        return parsed.returnUrl;
    }

    postCredentialsToServer(formValues) {
        // Temporary hard-coding of values. Learning one thing at a time -
        // right it is learning Axios, not React external configuration.
        const oauthTokenUrl = "https://localhost:44373/connect/token";

        const params = {
            client_id: formValues.emailAddress,
            client_secret: formValues.password,
            // Long term not going to be using client credentials with this form.
            // Again, focused on learning interfaces at the moment, then
            // will straighten this out.
            grant_type: "client_credential",
            scope: "admin"
        };

        Axios.post(oauthTokenUrl, QueryString.stringify(params))
            .then(function (response) {

                // TODO: parse response, get token(?), redirect to correct URL.
                // This redirect thing only makes sense if hosting a central
                // server that supports multiple applications. But standing
                // up an OAuth2 server is probably not the real goal here.
                // Rethink this. Might be appropriate to just use ASP.NET
                // Identity. Still would like to use JWT instead of Cookies.

                console.info(response);
            })
            .catch(function (error) {

                // TODO: something meaningful =D

                console.error(error);
            });
    }

    render() {
        return (
            <Container>
                <h1>Sign-in</h1>
                <Formik
                    initialValues={{ emailAddress: "", password: "", rememberMe: false, returnUrl: this.getReturnUrl() }}
                    onSubmit={values => {
                        alert(JSON.stringify(values, null, 2));
                        
                        this.postCredentialsToServer(values);
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