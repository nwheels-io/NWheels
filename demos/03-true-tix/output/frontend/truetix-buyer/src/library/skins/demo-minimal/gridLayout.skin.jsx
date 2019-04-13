import React, { Component } from "react";

export const GridLayout = (props) => (
    <div className="css-grid">
        {props.children}
    </div>
)

export const GridCell = (props) => (
    <div style={{gridColumn: props.col, gridRow: props.row}}>
        {props.children}
    </div>
)
