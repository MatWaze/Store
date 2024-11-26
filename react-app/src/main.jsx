import { StrictMode, useState, useEffect } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import SpeechToText from './SpeechToText.jsx';

function CartButtonEn() {
    const [cart, setCart] = useState({
        totalItems: 0,
        totalValue: '$0.00'
    });

    const updateCart = (totalItems, totalValue) => {
        setCart({
            totalItems: parseInt(totalItems, 10),
            totalValue: totalValue
        });
    };
    const [returnUrl, setReturnUrl] = useState('');

    useEffect(() => {
        window.updateCart = updateCart;
        const currentUrl = window.location.href;
        setReturnUrl(currentUrl);
        const cartButtonElement = document.querySelector('#cart-button');
        const totalItems = cartButtonElement.getAttribute('data-cart-items');
        const totalValue = cartButtonElement.getAttribute('data-cart-total');
        setCart({
            totalItems: parseInt(totalItems, 10),
            totalValue: totalValue
        });
    }, []);

    return (
        <div className="">
            <small className="navbar-text">
                <b>{"Your cart"}:</b> {cart.totalItems} {"item(s)"} {cart.totalValue}
            </small>
            <a className="btn btn-md btn-secondary navbar-btn mt-2"
                href={`/Cart/Cart?returnUrl=${encodeURIComponent(returnUrl)}`}>
                <i className="fa fa-shopping-cart"></i>
            </a>
        </div>
    );
}

function CartButtonRu() {
    const [cart, setCart] = useState({
        totalItems: 0,
        totalValue: '$0.00'
    });

    const updateCart = (totalItems, totalValue) => {
        setCart({
            totalItems: parseInt(totalItems, 10),
            totalValue: totalValue
        });
    };
    const [returnUrl, setReturnUrl] = useState('');

    useEffect(() => {
        window.updateCart = updateCart;
        const currentUrl = window.location.href;
        setReturnUrl(currentUrl);
        const cartButtonElement = document.querySelector('#cart-button-ru');
        const totalItems = cartButtonElement.getAttribute('data-cart-items');
        const totalValue = cartButtonElement.getAttribute('data-cart-total');
        setCart({
            totalItems: parseInt(totalItems, 10),
            totalValue: totalValue
        });
    }, []);

    return (
        <div className="">
            <small className="navbar-text">
                <b>{"Ваша корзина"}:</b> {cart.totalItems} {"товар(ов)"} {cart.totalValue}
            </small>
            <a className="btn btn-md btn-secondary navbar-btn mt-2"
                href={`/Cart/Cart?returnUrl=${encodeURIComponent(returnUrl)}`}>
                <i className="fa fa-shopping-cart"></i>
            </a>
        </div>
    );
}



const cartButtonElement = document.querySelector("#cart-button");
const cartButtonElementRu = document.querySelector("#cart-button-ru");

if (cartButtonElement) {
    createRoot(cartButtonElement).render(
        <CartButtonEn />
    );
}

if (cartButtonElementRu) {
    createRoot(cartButtonElementRu).render(
        <CartButtonRu />
    );
}

const speechButton = document.getElementById("speech-to-text-react");
createRoot(speechButton).render(
    <SpeechToText />
)
