import React, { Fragment } from 'react';
import classnames from 'classnames'

import * as Spreadsheet from './../../../components/Spreadsheet'
import './style.css';

const DEFAULT_COLUMN_WIDTH = 100
const ROW_STATE_NEW = 'NEW'

const renderHeaderCell = (column) => (
    <Fragment>
        {column.title}
    </Fragment>
)

const renderDataCell = (column, item) => {
    if (!item || !column.field || !item[column.field]) {
        return (<Fragment>&nbsp;</Fragment>)
    }
    return (
        <Fragment>
            {item[column.field]}
        </Fragment>
    )
}

const renderDataRow = (columns, item, state) => (
    <tr>
        {columns.map(column => (
            <td className={`type-${column.type} ${state ? 'state-' + state : ''}`}>
                {renderDataCell(column, item)}
            </td>
        ))}
        <td className="filler">&nbsp;</td>
    </tr>
)

export default (props, actions, context) => {

    const totalWidth = props.columns
        .map(c => c.width || DEFAULT_COLUMN_WIDTH)
        .reduce((prev, next) => prev + next)

    return (
        <div className="Component_Spreadsheet">
            <table>
                <thead>
                    <tr>
                        {props.columns.map(column => (
                            <td style={{ width: (column.width || DEFAULT_COLUMN_WIDTH) + 'px' }}>
                                {renderHeaderCell(column)}
                            </td>
                        ))}
                        <td className="filler" style={{ width: `calc(100% - ${totalWidth}px)` }}>&nbsp;</td>
                    </tr>
                </thead>
                <tbody>
                    {props.items.map(item => renderDataRow(props.columns, item))}
                    {renderDataRow(props.columns, undefined, ROW_STATE_NEW)}
                </tbody>
            </table>
            <div>
                <a onClick={() => actions.appendRow()}>Add</a>
            </div>
        </div>
    )
}
