<!DOCTYPE html>
<html lang="en">
<!-- <head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

<title>ILoveParts README</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            line-height: 1.6;
            margin: 0;
            padding: 20px;
        }
        /* h1, h2, h3 {
            color: #;
        } */
        table {
            border-collapse: collapse;
            width: 100%;
            margin: 20px 0;
        }
        th, td {
            border: 1px solid #ccc;
            padding: 10px;
            text-align: center;
        }
        th {
            background-color: #f8f8f8;
            color: #000
        }
        .note {
            background-color: #fff;
            border-left: 5px solid #2196f3;
            margin: 20px 0;
            padding: 10px;
        }
        .note > p { 
            color: #000
        }
    </style>
</head> -->
<body>
<header>
    <p align="center">
        <img width="250px" src="https://dontknowhowtonameit1.blob.core.windows.net/web/wwwroot/images/logo_iparts.png" alt="Logo">
    </p>
    <h1>ILoveParts - README</h1>
    <p>
        Welcome to <strong>ILoveParts</strong>, an e-commerce web application where users can find spare parts for various needs. The app is hosted on <a href="https://www.heroku.com/" target="_blank">Heroku</a> 
        and is accessible at <a href="https://iparts.me" target="_blank">iparts.me</a> (not always active).
    </p>
</header>

<section>
    <h2>Key Features</h2>
    <ul>
        <li><strong>User Registration:</strong> Users have to be registered in order to use the web app.</li>
        <li><strong>Profile:</strong>
        Users can update their basic information like name, set the address information which will be used for order processing, and view orders.
        </li>
        <li><strong>Localization:</strong>
        English and Russian depending on the user's locale.
        </li>
        <li><strong>Search Form:</strong> Users can search for parts using a search form. Results are fetched from eBay.</li>
        <li><strong>Cart:</strong> Users can add items to their cart, view the cart, and proceed to checkout.</li>
        <li><strong>Checkout:</strong> Two payment options are available: Braintree and YooKassa, both in test mode.</li>
        <li><strong>Email Notifications:</strong> Users receive email notifications in either English or Russian when they register or when an order payment is successful.</li>
    </ul>
</section>

<section>
    <h2>Payment Gateway Information</h2>
    <section>
        <h3>Payment Gateway Notes</h3>
        <p>
            <strong>YooKassa:</strong> This payment gateway requires OAuth authentication. In order to process payments in your test store, 
            you need to set up a YooKassa account and provide required credentials to your test store. 
            This is necessary for both production and test environments.
        </p>
        <p>
            <strong>Braintree:</strong> Unlike YooKassa, Braintree does not require OAuth authentication for production. 
            It only requires the necessary API keys for integration and does not involve additional account setup for the test mode.
        </p>
    </section>
    <h3>Payment Testing</h3>
    <div class="note">
        <p>
            This application is currently in <strong>test mode</strong>. All payments are simulated, allowing you to safely test 
            the checkout process. Below are the test cards for Braintree and YooKassa:
        </p>
    </div>

<h3>Braintree Test Cards</h3>
<table>
    <thead>
        <tr>
            <th>Card Number</th>
            <th>Card Type</th>
        </tr>
    </thead>
    <tbody>
        <tr><td>4111111111111111</td><td>Visa</td></tr>
        <tr><td>371449635398431</td><td>American Express</td></tr>
        <tr><td>6304000000000000</td><td>Maestro</td></tr>
        <tr><td>5555555555554444</td><td>Mastercard</td></tr>
        <tr><td>2223000048400011</td><td>Mastercard</td></tr>
    </tbody>
</table>

<h3>YooKassa Test Cards</h3>
<table>
    <thead>
        <tr>
            <th>Card Number</th>
            <th>Card Type</th>
        </tr>
    </thead>
    <tbody>
        <tr><td>5555555555554477</td><td>Mastercard</td></tr>
        <tr><td>5555555555554444</td><td>Mastercard</td></tr>
        <tr><td>6759649826438453</td><td>Maestro</td></tr>
        <tr><td>4793128161644804</td><td>Visa</td></tr>
        <tr><td>4111111111111111</td><td>Visa</td></tr>
    </tbody>
</table>
<p><em>Use any valid expiration date (e.g., 12/30) and any random CVV code.</em></p>
</section>


<section>
    <h2>Technologies Used</h2>
    <ul>
        <li><strong>ASP.NET Core:</strong> Backend and application logic.</li>
        <li><strong>React:</strong> Frontend components for dynamic features like cart updates and voice-enabled search.</li>
        <li><strong>Redis:</strong> Caching layer for improving search performance.</li>
        <li><strong>Braintree & YooKassa:</strong> Payment gateways for checkout.</li>
        <li><strong>Azure AI Services:</strong> Azure AI Speech Service provides voice recognition for the search feature to allow users to search by speaking (supports English only). Azure AI Translator translates content dynamically to support multiple languages, including English and Russian.</li>
        <li><strong>eBay API:</strong> Fetches spare parts search results using the eBay Browse and Translation API to offer products in Russian.</li>
        <li><strong>Heroku:</strong> Hosting environment.</li>
        <li><strong>PosrgreSQL:</strong>
        Storing user and product related data. 
    </ul>
</section>
</body>
</html>
