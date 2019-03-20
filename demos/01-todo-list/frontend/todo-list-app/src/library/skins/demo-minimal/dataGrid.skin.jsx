import React, { Component } from "react";

export class DataGrid extends Component {
    constructor(props) {
        super(props);
        this.handleNewItem = this.handleNewItem.bind(this);
    }

    componentDidMount() {
        this.props.pushAddItem && this.props.pushAddItem.addListener(this.handleNewItem);
        if (!this.props.isLoaded) {
            this.props.beginLoadItems(this.props.dal, this.getFields());
        }
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
                        {this.props.columns.map((col, index) => this.renderCellTH(index, col))}
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
                                    onClick={() => this.props.beginCommitItem(this.props.dal, item.key, {}, true)}
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
        this.props.beginCommitItem(this.props.dal, this.props.nextKey, data, false);
    }

    renderCellTH(colIndex, col) {
        return (
            <th key={colIndex}>
                {col.title}
                <br/>
                {this.renderColumnSortBox(colIndex, col)}
                <br />
                {this.renderColumnFilterBox(colIndex, col)}
            </th>
        );
    }

    renderColumnSortBox(colIndex, col) {
        const sortBoxValue = this.getColumnSortBoxValue(this.props.columnSort[col.field]);

        return (
            <select 
                defaultValue={sortBoxValue}
                onChange={e => this.props.setColumnSort(this.props.dal, this.getFields(), col.field, this.getColumnSortBoxAscending(e.target.value))}
            >
                <option value="">- no sort -</option>
                <option value="ASC">ASC</option>
                <option value="DESC">DESC</option>
            </select>
        );
    }

    renderColumnFilterBox(colIndex, col) {
        if (col.type !== 'key') {
            return (
                <div>
                    {this.renderCellEditor(-1, colIndex, col, { data: this.props.columnFilter }, { isFilter: true })}
                    <button onClick={e => this.props.setColumnFilter(this.props.dal, this.getFields(), col.field, undefined)}>x</button>
                </div>
            );
        } 
        return null;
    }

    renderCellTD(rowIndex, colIndex, col, item) {
        const selection = this.props.selectedCell;
        const isEditMode = (
            item.state === 'UNCHANGED' &&
            !!col.editor &&
            selection && 
            selection.row === rowIndex && 
            selection.col === colIndex);
        
        return (
            <td key={colIndex} onClick={() => this.props.selectCell(rowIndex, colIndex)}>
                {(isEditMode 
                    ? this.renderCellEditor(rowIndex, colIndex, col, item)
                    : this.renderCellValue(rowIndex, colIndex, col, item)                 
                )}
            </td>
        );
    }

    renderCellValue(rowIndex, colIndex, col, item) {
        switch (col.type) {
            case 'bool':
                if (item.state === 'UNCHANGED' && col.editor === 'check') {
                    return this.renderCellEditor(rowIndex, colIndex, col, item);
                } else {
                    return (<input type='checkbox' defaultChecked={!!item.data[col.field]} disabled={true} />); 
                }
            default:
                return item.data[col.field] || '';
        }
    }

    renderCellEditor(rowIndex, colIndex, col, item, options) {
        switch (col.editor) {
            case 'text':    
                return (<input 
                    type='text' 
                    defaultValue={item.data[col.field] || ''} 
                    onBlur={(e) => {
                        const newValue = e.target.value;
                        this.commitCellEditorValue(col, item, options, newValue);
                    }}
                />);
            case 'check':    
                if (options && options.isFilter) {
                    return (
                        <select 
                            defaultValue={JSON.stringify(item.data[col.field]) || ''}
                            onChange={e => {
                                const filterValue = (e.target.value === '' ? undefined : JSON.parse(e.target.value));
                                this.commitCellEditorValue(col, item, options, filterValue);
                            }}
                        >
                            <option value=''>- no filter -</option>
                            <option value='true'>YES</option>
                            <option value='false'>NO</option>
                        </select>
                    );
                } else {
                    return (<input 
                        type='checkbox' 
                        defaultChecked={!!item.data[col.field]} 
                        onClick={(e) => {
                            const newValue = e.target.checked;
                            this.commitCellEditorValue(col, item, options, newValue);
                        }}
                    />);
                }
        }
    }

    commitCellEditorValue(col, item, options, newValue) {
        if (newValue !== item.data[col.field]) {
            if (options && options.isFilter) {
                this.props.setColumnFilter(this.props.dal, this.getFields(), col.field, newValue);
            } else {
                this.props.beginCommitItem(this.props.dal, item.key, {[col.field]: newValue}, false);
            }
        }
    }

    getColumnSortBoxAscending(optionValue) {
        switch (optionValue) {
            case 'ASC': return true;
            case 'DESC': return false;
            default: return undefined;
        }
    }

    getColumnSortBoxValue(ascending) {
        if (typeof ascending !== 'boolean') {
            return '';
        }
        return (ascending ? 'ASC' : 'DESC');
    }

    getFields() {
        return this.props.columns.map(col => col.field);
    }
}

