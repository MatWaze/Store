import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

// Translations
const resources = {
    en: {
        translation: {
            "Your cart": "Your cart",
            "item(s)": "item(s)",
            "Cart Button": "Cart Button",
        }
    },
    ru: {
        translation: {
            "Your cart": "Ваша корзина",
            "item(s)": "товар(ов)",
            "Cart Button": "Кнопка корзины",
        }
    }
};

i18n
    .use(initReactI18next)
    .init({
        resources,
        lng: navigator.language || 'en', // Automatically set the language based on user's locale
        fallbackLng: 'en', // Fallback language if the user's locale is not available
        interpolation: {
            escapeValue: false // React already escapes content
        }
    });

export default i18n;
