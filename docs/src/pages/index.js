import React from 'react';
import clsx from 'clsx';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import styles from './index.module.css';
import HomepageFeatures from '../components/HomepageFeatures';
import CarbonAd from '../js/carbon-ad';

// The docs home (work item 26112202, D-014): a real landing page in place of the old client-side redirect, so the
// site root has its own title, description and heading. The navbar "Docs" link still opens the getting-started page.
const pageTitle = 'Blazor components for Bootstrap 5.3';
const pageDescription = 'Docs for Blazor Bootstrap, free Bootstrap 5.3 components for Blazor Server, WebAssembly and MAUI on .NET 8 to 11: setup, layout and every component API.';

function HomepageHeader() {
    const { siteConfig } = useDocusaurusContext();
    return (
        <header className={clsx('hero hero--primary', styles.heroBanner)}>
            <div className="container">
                <img className={styles.mb_3} src={siteConfig.customFields.logo_white} width="200" alt="" />
                <h1 className="hero__title">Blazor Bootstrap documentation</h1>
                <p className="hero__subtitle">
                    Free, open-source Bootstrap 5.3 components for Blazor Server, Blazor WebAssembly and .NET MAUI Blazor Hybrid
                    apps on .NET 8, 9, 10 and 11. Install the NuGet package, follow a getting-started guide, then look up any
                    component's parameters, methods and events.
                </p>
                <div className={styles.buttons}>
                    <Link className="button button--secondary button--lg" to="/getting-started/blazor-webassembly-net-11">
                        Get started with .NET 11
                    </Link>
                    <Link className="button button--outline button--secondary button--lg" to="/getting-started/blazor-webassembly-net-10">
                        Get started with .NET 10
                    </Link>
                </div>
            </div>
        </header>
    );
}

export default function Home() {
    return (
        <Layout title={pageTitle} description={pageDescription}>
            <HomepageHeader />
            <main>
                <div className="container">
                    <CarbonAd />
                </div>
                <HomepageFeatures />
            </main>
        </Layout>
    );
}
