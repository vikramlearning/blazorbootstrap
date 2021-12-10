import React from 'react';
import clsx from 'clsx';
import styles from './HomepageFeatures.module.css';

const FeatureList = [
    {
        title: 'Easy to Use',
        emoji: '👍',
        description: (
            <>
                BlazorBootstrap has been built with Blazor and Bootstrap CSS framework to use Bootstrap components with ease.
            </>
        ),
    },
    {
        title: 'Focus on What Matters',
        emoji: '🧐',
        description: (
            <>
                BlazorBootstrap lets you focus on your deliverables, and we'll build the Bootstrap components in Blazor.
            </>
        ),
    },
    {
        title: 'Open-source & free',
        emoji: '🔓',
        description: (
            <>
                BlazorBootstrap is licensed under the Apache License 2.0. Clone it, fork it, and customize it.
            </>
        ),
    },
];

function Feature({ emoji, title, description }) {
    return (
        <div className={clsx('col col--4')}>
            <div className="text--center">
                <span className="feature-emoji">{emoji}</span>
            </div>
            <div className="text--center padding-horiz--md">
                <h3>{title}</h3>
                <p>{description}</p>
            </div>
        </div>
    );
}

export default function HomepageFeatures() {
    return (
        <section className={styles.features}>
            <div className="container">
                <div className="row">
                    {FeatureList.map((props, idx) => (
                        <Feature key={idx} {...props} />
                    ))}
                </div>
            </div>
        </section>
    );
}
