import React from 'react';
import classnames from 'classnames'

import * as Spreadsheet from './../../../components/Spreadsheet'
import './style.css';

export const UIRender = (props, actions, context) => (
    <div className="Component_Spreadsheet">
        <table>
            <thead>
                <tr>
                    <td>Key</td>
                    <td>Value</td>
                </tr>
            </thead>
            <tbody>
                {props.items.map(item => (
                    <tr>
                        <td>{item.key}</td>
                        <td>{item.value}</td>
                    </tr>
                ))}
            </tbody>
        </table>
        <div>
            <a onClick={() => actions.appendRow()}>Add</a>
        </div>
    </div>
)
