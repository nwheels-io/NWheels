import React, { Component } from 'react';
import logo from './logo.svg';
import './App.css';

class Counter extends Component {
    constructor(props) {
        super(props);
        this.state = {
            currentValue: props.initialValue,
            incrementCount: 0,
            decrementCount: 0
        };
    }

    render() {
        return (
            <React.Fragment>
                <span>Initial:</span>
                <span><strong>{this.props.initialValue}</strong></span>
        
                <span>Current:</span>
                <span><strong>{this.state.currentValue}</strong></span>
        
                <span>Delta:</span>
                <span><strong>{this.state.currentValue - this.props.initialValue}</strong></span>
        
                <button onClick={() => this.handleIncrement()}>Increment ({this.state.incrementCount})</button>
                <button onClick={() => this.handleDecrement()}>Decrement ({this.state.decrementCount})</button>
            </React.Fragment>
        )
    }

    handleIncrement() {
        this.setState({ 
            currentValue: this.state.currentValue + this.props.step,
            incrementCount: this.state.incrementCount + 1
        })
    }

    handleDecrement() {
        this.setState({ 
            currentValue: this.state.currentValue - this.props.step,
            decrementCount: this.state.decrementCount + 1
        })
    }
}

class App extends Component {
    constructor(props) {
        super(props);
        this.state = { }
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
                <table>
                    <tr>
                        <td>First counter</td>
                        <td><Counter initialValue={123} step={1} /></td>
                    </tr>
                    <tr>
                        <td>Second counter</td>
                        <td><Counter initialValue={456} step={2} /></td>
                    </tr>
                </table>
            </div>
        );
    }
}

export default App;
