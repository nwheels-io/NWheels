import React, { Component } from "react";
import { throws } from "assert";

export class DataGrid extends Component {
    constructor(props) {
        super(props);
        this.handleNewItem = this.handleNewItem.bind(this);
    }

    componentDidMount() {
        this.props.pushAddItem && this.props.pushAddItem.addListener(this.handleNewItem);
        this.props.beginLoadItems();
    }

    componentWillUnmount() {
        this.props.pushAddItem && this.props.pushAddItem.removeListener(this.handleNewItem);
    }

    render() {
        if (!this.props.isLoaded) {
            return (
                <div>
                    {(this.props.isLoadFailed ? 'LOAD FAILED!' : 'LOADING....')}
                </div>
            );
        }

        return (
            <table>
                <thead>
                    <tr>
                        <th>KEY</th>
                        <th>STATE</th>
                        {this.props.columns.map((col, index) => (
                            <th key={index}>{col.title}</th>
                        ))}
                    </tr>
                </thead>
                <tbody>
                    {this.props.items.map(item => (
                        <tr key={item.key}>
                            <td>{item.key}</td>
                            <td>{item.state}</td>
                            {this.props.columns.map((col, index) => (
                                <td key={index}>{item.data[col.field] || ''}</td>
                            ))}
                        </tr>
                    ))}
                </tbody>
            </table>
        );
    }

    handleNewItem(data) {
        this.props.addItem(data)
    }
}

