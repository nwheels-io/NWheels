import React from 'react';
import { render } from 'react-dom'
import PropTypes from 'prop-types'

import * as Counter from './Counter/render'

export class AscetTheme extends React.Component {
    getChildContext() {
        return {
            theme: {
                render: {
                    Demo: () => (
                        <div style={{border: '1px solid black', padding:'3px', margin: '3px'}}>
                            This is the ASCET theme!
                        </div>
                    ),
                    Counter: Counter.UIRender
                }
            }
        };
    }
    render() {
        return this.props.children
    }
}
AscetTheme.childContextTypes = {
    theme: PropTypes.object.isRequired
}
