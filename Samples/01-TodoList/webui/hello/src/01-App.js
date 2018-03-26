import React, { Component } from 'react';
import logo from './logo.svg';
import './App.css';


const Counter = ({ value, onIncrement, onDecrement }) => (
    <div>
        <span>Counter value:</span>
        <span>{value}</span>
        <button onClick={onIncrement}>Increment</button>
        <button onClick={onDecrement}>Decrement</button>
    </div>
)

class App extends Component {
    constructor(props) {
        super(props);
        this.state = {
            initialValue: 101,
            counterValue: 456
        };
    }

    render() {
        return (
            <div className="App">
                <header className="App-header">
                    <img src={logo} className="App-logo" alt="logo" />
                    <h1 className="App-title">Welcome to React</h1>
                </header>
                <p className="App-intro">
                    To get started, edit <code>src/App.js</code> and save to reload.
                </p>
                <p>
                    Initial value: {this.state.initialValue}                    
                </p>
                <p>
                    Delta value: {this.state.counterValue - this.state.initialValue}                    
                </p>
                <Counter
                    value={this.state.counterValue}
                    onIncrement={() => this.handleIncrement()}
                    onDecrement={() => this.handleDecrement()} />
            </div>
        );
    }

    handleIncrement() {
        this.setState({ counterValue: this.state.counterValue + 1 })
    }

    handleDecrement() {
        this.setState({ counterValue: this.state.counterValue - 1 })
    }
}

export default App;
