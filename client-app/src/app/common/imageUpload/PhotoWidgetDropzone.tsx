import { useCallback } from 'react'
import { useDropzone } from 'react-dropzone'
import { Header, Icon } from 'semantic-ui-react';

interface Props {
    setFiles: (files: any) => void; // void function 
}

export default function PhotoWidgetDropzone({ setFiles }: Props) {
    // for dropzone active
    const dzStyles = {
        border: "dashed 3px #eee",
        borderColor: "#eee",
        borderRadius: "5px",
        paddingTop: "30px",
        textAlign: "center" as "center",
        height: 200,
    };
    // for dropzone inactive
    const dzActive = {
        borderColor: "green"
    };

    const onDrop = useCallback(
        (acceptedFiles: any[]) => {
            setFiles(
                acceptedFiles.map((file: any) =>
                    Object.assign(file, {
                        preview: URL.createObjectURL(file), //preview of the image placed in dz (in client memory)
                    })
                )
            );
        },
        [setFiles]
    );

    const { getRootProps, getInputProps, isDragActive } = useDropzone({ onDrop })

    return (
        <div {...getRootProps()} style={isDragActive ? { ...dzStyles, ...dzActive } : dzStyles}>
            <input {...getInputProps()} />
            <Icon name='upload' size='huge' />
            <Header content='Drop image here' />
        </div>
    )
}