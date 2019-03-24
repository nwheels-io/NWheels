import React, { Component } from "react";

const CellEditor = ({rowIndex, colIndex, col, item, isFilter, onCommitValue}) => {

    function commitValue(newValue) {
        if (newValue !== item.data[col.field]) {
            onCommitValue(newValue);
            // if (options && options.isFilter) {
            //     this.props.setColumnFilter(this.props.dal, this.getFields(), col.field, newValue);
            // } else {
            //     this.props.beginCommitItem(this.props.dal, item.key, {[col.field]: newValue}, false);
            // }
        }
    }

    switch (col.editor) {
        case 'text':    
            return (<input 
                type='text' 
                defaultValue={item.data[col.field] || ''} 
                onBlur={(e) => commitValue(e.target.value)}
            />);
        case 'check':    
            if (isFilter) {
                return (
                    <select 
                        defaultValue={JSON.stringify(item.data[col.field]) || ''}
                        onChange={e => {
                            const filterValue = (e.target.value === '' ? undefined : JSON.parse(e.target.value));
                            commitValue(filterValue);
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
                    onClick={(e) => commitValue(e.target.checked)}
                />);
            }
        default:
            return (<span>(unsupported)</span>);
    }

};

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

        const RowComponent = this.props.getRowComponent();

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
                        <RowComponent 
                            key={item.key}
                            stateID={this.props.stateID}
                            dal={this.props.dal}
                            columns={this.props.columns}
                            rowIndex={rowIndex}
                        />
                    ))}
                </tbody>
            </table>
        );
    }

    handleNewItem(data) {
        this.props.beginCommitNewItem(this.props.dal, data);
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
                    <CellEditor 
                        rowIndex={-1} 
                        colIndex={colIndex} 
                        col={col} 
                        item={{ data: this.props.columnFilter }} 
                        isFilter={true}
                        onCommitValue={newValue => this.props.setColumnFilter(this.props.dal, this.getFields(), col.field, newValue)} 
                    />
                    <button onClick={e => this.props.setColumnFilter(this.props.dal, this.getFields(), col.field, undefined)}>x</button>
                </div>
            );
        } 
        return null;
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

    static get rowComponent() {
        return DataGridRow;
    }
}

class DataGridRow extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        const item = this.props.item;

        return (
            <tr key={item.key}>
                <td>{item.key}</td>
                <td>{item.state}</td>
                {this.props.columns.map((col, colIndex) => {
                    return this.renderCellTD(this.props.rowIndex, colIndex, col, item);
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
        );
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

    renderCellEditor(rowIndex, colIndex, col, item) {
        return (
            <CellEditor 
                rowIndex={rowIndex} 
                colIndex={colIndex} 
                col={col} 
                item={item} 
                isFilter={false}
                onCommitValue={newValue => this.props.beginCommitItem(this.props.dal, item.key, {[col.field]: newValue}, false)} 
            />
        )
    }
}
