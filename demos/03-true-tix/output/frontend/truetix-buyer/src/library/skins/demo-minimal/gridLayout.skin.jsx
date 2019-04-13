import React, { Component } from "react";

export const GridLayout = (props) => (
    <table>
        <tbody>
            {props.matrix.map(row => (
                <tr>
                    {row.map(cell => (
                        <td>
                            {cell}
                        </td>
                    ))}
                </tr>
            ))}    
        </tbody>
    </table>
);
