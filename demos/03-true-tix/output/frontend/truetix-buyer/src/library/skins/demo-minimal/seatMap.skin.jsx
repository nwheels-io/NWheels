import React, { Component } from "react";

export const SeatMap = (props) => (
    <div style={{textAlign:'center'}}>
        {props.rows.map((row, index) => (
            <table key={index}>
                <tbody>
                    <tr>
                        {row.seats.map(seat => (
                            <td key={seat.id}>
                                [<a onClick={() => props.selectSeat(seat)}>#{seat.id} : {seat.label}</a>]
                            </td>
                        ))}
                    </tr>
                </tbody>
            </table>
        ))}    
        <div style={{margin:'10px',backgroundColor:'gray'}}>
            Legend
            <ul>
                {Object.keys(props.colors).map(key => (
                    <li key={key}>
                        <div style={{display:'inline-block', width:'50px', height:'20px', backgroundColor: props.colors[key]}}></div>
                        <span>{key}</span>
                    </li>
                ))}
            </ul>
        </div>
    </div>
);
