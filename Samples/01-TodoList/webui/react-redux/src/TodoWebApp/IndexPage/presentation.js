import React from 'react'
import PropTypes from 'prop-types'

const Item = ({text, icon}) => (
    <span>
        {text}
    </span>
)

const ToggleOption = ({value, isSelected, children}) => {
    if (isSelected) {
        return children()
    } 

    return (
        <a  
            href="#" 
            onClick={e => {
                e.preventDefault()
                onClick()
            }}
        >
            {children}
        </a>
    )
}

const ToggleGroup = ({selectedIndex, onChange, children}) => (
    <ul>
        {React.Children.map(this.props.children, (child, index) => {
            return (
                <ToggleOption value=    

            )
            const clonedChild = React.cloneElement(child, {
                isSelected: index == selectedIndex
            })
        })}
    </ul>
)

const Toolbar = (doneOption) => (
    <div>
        Show:
        <ToggleGroup>
            <Option value={null}>All</Option>
            <Option value={false}>Active</Option>
            <Option value={true}>Completed</Option>
        </ToggleGroup>
    </div>
)

const IndexPage = () => (
    <div>
        <Toolbar />
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
)


export default IndexPage;
