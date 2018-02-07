import React from 'react';
import { Route, Link } from 'react-router-dom'
import HomePage from './HomePage/render'

const HelloWorldApp = () => (
    <div>
        <header>
            <Link to="/">Home</Link>
        </header>

        <main>
            <Route exact path="/" component={HomePage} />
        </main>
    </div>
)

export default HelloWorldApp
