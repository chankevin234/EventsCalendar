import { Message } from "semantic-ui-react";

interface Props {
    errors: any;
}

export default function ValidationErrors({errors}: Props) {
    return (

        <Message error>
            {errors && (
                <Message.List>
                    {errors.map((err: any, index: any) => (
                        <Message.Item key={index}>{err}</Message.Item>
                    ))}
                </Message.List>
            )}
        </Message>
    )
}