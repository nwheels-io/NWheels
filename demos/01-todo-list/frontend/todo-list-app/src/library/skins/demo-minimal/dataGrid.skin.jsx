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
                        <th>ACTIONS</th>
                    </tr>
                </thead>
                <tbody>
                    {this.props.items.map((item, rowIndex) => (
                        <tr key={item.key}>
                            <td>{item.key}</td>
                            <td>{item.state}</td>
                            {this.props.columns.map((col, colIndex) => {
                                return this.renderCellTD(rowIndex, colIndex, col, item);
                            })}
                            <td>
                                <button
                                    disabled={item.state !== 'UNCHANGED'} 
                                    onClick={() => this.props.beginCommitItem(item.key, {}, true)}
                                >
                                    X
                                </button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        );
    }

    handleNewItem(data) {
        this.props.addItem(data);
        this.props.beginCommitItem(this.props.nextKey, data, false);
    }

    renderCellTD(rowIndex, colIndex, col, item) {
        const selection = this.props.selectedCell;
        const isEditMode = (
            item.state === 'UNCHANGED' &&
            selection && 
            selection.row === rowIndex && 
            selection.col === colIndex);
        
        return (
            <td key={colIndex} onClick={() => this.props.selectCell(rowIndex, colIndex)}>
                {(isEditMode 
                    ? this.renderCellEditor(rowIndex, colIndex, col, item)
                    : this.renderCellValue(rowIndex, colIndex, col, item)                 )}
            </td>
        );
    }

    renderCellValue(rowIndex, colIndex, col, item) {
        return item.data[col.field] || '';
    }

    renderCellEditor(rowIndex, colIndex, col, item) {
        return (<input 
            type='text' 
            defaultValue={item.data[col.field] || ''} 
            onBlur={(e) => {
                const newValue = e.target.value;
                if (newValue !== item.data[col.field]) {
                    this.props.beginCommitItem(item.key, {[col.field]: newValue}, false);
                }
            }}
        />);
    }
}

