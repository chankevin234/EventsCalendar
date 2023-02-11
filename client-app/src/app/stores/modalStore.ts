import { makeAutoObservable } from "mobx"

interface Modal { //a modal is made up of these two props
    open: boolean;
    body: JSX.Element | null;
}

export default class ModalStore {
    modal: Modal = {
        open: false,
        body: null
    } //initial modal obj

    constructor() {
        makeAutoObservable(this); //makes this class observable by mobx
    }

    openModal = (content: JSX.Element) => {
        this.modal.open = true; 
        this.modal.body = content;
    }

    closeModal = () => {
        this.modal.open = false;
        this.modal.body = null;
    }
}