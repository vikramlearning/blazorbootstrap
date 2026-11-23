import React from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import styles from './HomepageFeatures.module.css';

// Where to go next from the docs home (work item 26112202, AC-17).
const SectionList = [
    {
        title: 'Components',
        to: '/components/accordion',
        description: 'Parameters, methods and events for Grid, Modal, Sidebar, Tabs and every other component.',
    },
    {
        title: 'Forms',
        to: '/forms/autocomplete',
        description: 'Inputs that work with EditForm validation: text, number, date, file, AutoComplete and more.',
    },
    {
        title: 'Live demos',
        to: 'https://demos.blazorbootstrap.com/',
        description: 'Every component running in a Blazor app, with the source code for each example.',
    },
    {
        title: 'Blog',
        to: '/blog',
        description: 'Release notes and upgrade guides for each version of Blazor Bootstrap.',
    },
];

function Section({ title, to, description }) {
    return (
        <div className={clsx('col col--3', styles.section)}>
            <h2 className={styles.sectionTitle}>
                <Link to={to}>{title}</Link>
            </h2>
            <p>{description}</p>
        </div>
    );
}

export default function HomepageFeatures() {
    return (
        <section className={styles.features} aria-label="Explore the docs">
            <div className="container">
                <div className="row">
                    {SectionList.map((props) => (
                        <Section key={props.title} {...props} />
                    ))}
                </div>
            </div>
        </section>
    );
}
