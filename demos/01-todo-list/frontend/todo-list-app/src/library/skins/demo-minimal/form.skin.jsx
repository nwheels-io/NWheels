import React, { Component } from "react";

export class Form extends Component {
    constructor(props) {
        super(props);
        this.handleResetForm = this.handleResetForm.bind(this);
    }

    componentDidMount() {
        this.props.pushResetForm && this.props.pushResetForm.addListener(this.handleResetForm);
    }

    componentWillUnmount() {
        this.props.pushResetForm && this.props.pushResetForm.removeListener(this.handleResetForm);
    }

    render() {
        return (
            <div>
                <table>
                    <tbody>
                        {this.props.fields.map(field => (
                            <tr key={field.name}>
                                <td>{field.label}</td>
                                <td>
                                    <input 
                                        type='text' 
                                        value={this.props.values[field.name] || ''} 
                                        onChange={event => this.props.setValue(field.name, event.target.value)} 
                                    />
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                <div>
                    {this.props.actions.map((action, index) => (
                        <button 
                            key={action.name || index} 
                            onClick={() => action.onExecute(this.props.values)}
                        >
                            {action.label}
                        </button>
                    ))}
                </div>
            </div>
        );
    }

    handleResetForm(data) {
        this.props.resetForm();
    }
}
