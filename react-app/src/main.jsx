import { StrictMode, useState, useEffect } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'

// eslint-disable-next-line react-refresh/only-export-components

function CartButton() {
    const [cart, setCart] = useState({
        totalItems: 0,
        totalValue: '$0.00'
    });

    const updateCart = (totalItems, totalValue) =>
    {
        setCart({
            totalItems: parseInt(totalItems, 10),
            totalValue: totalValue
        });
    };
    const [returnUrl, setReturnUrl] = useState('');

    useEffect(() => {
        // Expose updateCart function globally so it can be called from outside
        // can call window inside Razor Page or View
        window.updateCart = updateCart;
        const currentUrl = window.location.href;
        setReturnUrl(currentUrl);
        // Access the cart data from the attributes if needed
        const cartButtonElement = document.querySelector('#cart-button');
        const totalItems = cartButtonElement.getAttribute('data-cart-items');
        const totalValue = cartButtonElement.getAttribute('data-cart-total');

        // Initialize state with current cart data
        setCart({
            totalItems: parseInt(totalItems, 10),
            totalValue: totalValue
        });
    }, []);

    return (
        <div className="">
            <small className="navbar-text">
                <b>Your cart:</b> {cart.totalItems} item(s) {cart.totalValue}
            </small>
            <a className="btn btn-md btn-secondary navbar-btn mt-2"
                href={`/Cart/Cart?returnUrl=${encodeURIComponent(returnUrl)}`}>
                <i className="fa fa-shopping-cart"></i>
            </a>
        </div>
    );
}

const cartButtonElement = document.querySelector("#cart-button");
if (cartButtonElement) {
    createRoot(cartButtonElement).render(
            <CartButton />
    );
}