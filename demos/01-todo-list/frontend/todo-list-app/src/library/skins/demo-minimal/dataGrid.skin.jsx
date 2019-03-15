import React, { Component } from "react";

export class DataGrid extends Component {
    constructor(props) {
        super(props);
        this.handleNewItem = this.handleNewItem.bind(this);
    }

    componentDidMount() {
        this.props.pushAddItem && this.props.pushAddItem.addListener(this.handleNewItem);
    }

    componentWillUnmount() {
        this.props.pushAddItem && this.props.pushAddItem.removeListener(this.handleNewItem);
    }

    render() {
        return (
            <table>
                <thead>
                    <tr>
                        {this.props.columns.map((col, index) => (
                            <th key={index}>{col.title}</th>
                        ))}
                    </tr>
                </thead>
                <tbody>
                    {this.props.items.map(item => (
                        <tr key={item['$key']}>
                            {this.props.columns.map((col, index) => (
                                <td key={index}>{item[col.field]}</td>
                            ))}
                        </tr>
                    ))}
                </tbody>
            </table>
        );
    }

    handleNewItem(data) {
        this.props.add(data)
    }
}

