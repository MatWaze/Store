//import React, { useState, useEffect } from 'react';

//export function CartButton
//{
//    return (
//        <div class="">
//            <small class="navbar-text">
//                <b>Your cart:</b>
//        @Model.Lines.Sum(x => x.Quantity) item(s)
//                @Model.ComputeTotalValue().ToString("c")
//            </small>
//            <a class="btn btn-md btn-secondary navbar-btn mt-2"
//                href="http://localhost5001/Cart/Cart" asp-route-returnurl=
//                "@ViewContext.HttpContext.Request.PathAndQuery()">
//                <i class="fa fa-shopping-cart"></i>
//            </a>
//        </div> 
//    );
//};

//export function CartWidget({ cart, onViewCart }) {
//    const [total, setTotal] = useState(0);

//    useEffect(() => {
//        if (cart && cart.lines) {
//            const totalAmount = cart.lines.reduce((sum, line) => sum + (line.quantity * line.product.price), 0);
//            setTotal(totalAmount);
//        }
//    }, [cart]);

//    return (
//        <div className="cart-widget" style={{ display: cart ? 'block' : 'none', position: 'absolute', background: 'white', border: '1px solid #ccc', zIndex: 1000 }}>
//            <h4>Your Cart</h4>
//            <table className="table table-bordered">
//                <thead>
//                    <tr>
//                        <th>Item</th>
//                        <th>Quantity</th>
//                        <th className="text-right">Price</th>
//                    </tr>
//                </thead>
//                <tbody>
//                    {cart.lines.length > 0 ? (
//                        cart.lines.map((line) => (
//                            <tr key={line.product.id}>
//                                <td>{line.product.name}</td>
//                                <td>{line.quantity}</td>
//                                <td className="text-right">{(line.quantity * line.product.price).toLocaleString('en-US', { style: 'currency', currency: 'USD' })}</td>
//                            </tr>
//                        ))
//                    ) : (
//                        <tr>
//                            <td colSpan="3">Your cart is empty.</td>
//                        </tr>
//                    )}
//                </tbody>
//            </table>
//            <div className="text-right">
//                <strong>Total: </strong>
//                <span>{total.toLocaleString('en-US', { style: 'currency', currency: 'USD' })}</span>
//            </div>
//            <div className="text-center">
//                <button className="btn btn-primary" onClick={onViewCart}>View Cart</button>
//            </div>
//        </div>
//    );
//};

//export { CartWidget };
