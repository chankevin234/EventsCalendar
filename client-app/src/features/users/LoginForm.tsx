import { ErrorMessage, Form, Formik } from "formik";
import { observer } from "mobx-react-lite";
import { Button, Header, Label } from "semantic-ui-react";
import MyTextInput from "../../app/common/form/MyTextInput";
import { useStore } from "../../app/stores/store";

export default observer(function LoginForm() {
    const {userStore} = useStore(); // when you add a store, make the function an 'observer'

    return (
        <Formik 
            initialValues={{email: '', password: '', error: null}}
            onSubmit={(values, {setErrors}) => userStore.login(values).catch(error => 
                setErrors({error: 'Invalid email or pass'}))} // if an error is caught, the error property is set as that string
        >
            {({handleSubmit, isSubmitting, errors}) => ( //pass down these "formik function into the "formik Form" tag
                <Form className="ui form" onSubmit={handleSubmit} autoComplete="off">
                    <Header as='h2' content='Login to Reactivities' color='teal' textAlign="center" />
                    <MyTextInput placeholder="Email" name="email" />
                    <MyTextInput placeholder="Password" name="password" type="password" />
                    <ErrorMessage 
                        name="error" 
                        render={() => 
                            <Label style={{marginBottom: 10}} basic color="red" content={errors.error} />}     
                    />
                    <Button loading={isSubmitting} positive content="Login" type="submit" fluid />
                </Form>
            )}

        </Formik>
    )
})