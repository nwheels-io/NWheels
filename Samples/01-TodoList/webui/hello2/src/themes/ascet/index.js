import React from 'react';
import { render } from 'react-dom'
import PropTypes from 'prop-types'

import * as Counter from './Counter'
import * as Spreadsheet from './Spreadsheet'

class ThemeRender {
    Demo() {
        return (
            <div style={{ border: '1px solid black', padding: '3px', margin: '3px' }}>
                This is the ASCET theme!
            </div>
        )
    }
    get Counter() { return Counter.UIRender }
    get Spreadsheet() { return Spreadsheet.UIRender }
}

export class AscetTheme extends React.Component {
    getChildContext() {
        return {
            theme: {
                render: new ThemeRender()
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
