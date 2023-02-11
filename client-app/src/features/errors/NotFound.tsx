import { Link } from "react-router-dom";
import { Button, Header, Icon, Segment } from "semantic-ui-react";

export default function NotFound() {
    return (
        <Segment placeholder> 
            <Header icon>
                <Icon name='search' />
                Oops - we can't find this!
            </Header>
            <Segment.Inline>
                <Button as={Link} to='/activities'>
                    Return to Activities pg
                </Button>
            </Segment.Inline>
        </Segment>
    )
}