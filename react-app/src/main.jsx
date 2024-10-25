import { StrictMode, useState, useEffect } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import { useTranslation } from 'react-i18next';
import './i18n'; // Import i18n configuration

function CartButton() {
    const { t } = useTranslation(); // Hook for localization
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
                <b>{t("Your cart")}:</b> {cart.totalItems} {t("item(s)")} {cart.totalValue}
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
