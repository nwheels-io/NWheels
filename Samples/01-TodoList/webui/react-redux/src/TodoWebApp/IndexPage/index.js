import React, { Component } from 'react';

class IndexPage extends Component {
    render() {
        return (
            <div>
                <div>
                    Show:
                    <a href="">All</a>
                    <a href="">Done</a>
                    <a href="">Not done</a>
                </div>
                <table>
                    <thead>
                        <tr>
                            <th>Description</th>
                            <th>Done</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>First item</td>
                            <td><input type="checkbox" /></td>
                            <td><a href="">Delete</a></td>
                        </tr>
                        <tr>
                            <td>Second item</td>
                            <td><input type="checkbox" /></td>
                            <td><a href="">Delete</a></td>
                        </tr>
                        <tr>
                            <td colspan="3"><a href="">New</a></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        );
    }
}

export default IndexPage;
