import React from 'react'; 
import { IDuck } from './demo';
//child component

interface IProps {
    duck: IDuck;
}

export default function DuckItem({duck}: IProps) {
    return (
        <div key={duck.name}>
            <span>{duck.name}</span>
            <button onClick={() => duck.makeSound(duck.name + ' quack')}>Make sound</button>
        </div>
    )
}