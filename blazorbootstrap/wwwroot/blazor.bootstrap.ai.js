export async function createChatCompletions(key, message, dotNetHelper) {
    const API_KEY = key;
    const API_URL = 'https://api.openai.com/v1/chat/completions';

    try {
        // Fetch the response from the OpenAI API with the signal from AbortController
        const response = await fetch(API_URL, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${API_KEY}`,
            },
            body: JSON.stringify({
                model: "gpt-3.5-turbo", //"gpt-4",
                messages: [message],
                max_tokens: 200,
                stream: true, // For streaming responses
            })
        });

        // Read the response as a stream of data
        const reader = response.body.getReader();
        const decoder = new TextDecoder("utf-8");
        let i = 0;
        while (true) {
            const { done, value } = await reader.read();
            if (done) {
                break;
            }

            // Massage and parse the chunk of data
            const chunk = decoder.decode(value);
            const lines = chunk.split("\n");

            for (const payload of lines) {

                if (payload.includes('[DONE]')) {
                    dotNetHelper.invokeMethodAsync('ChartCompletetionsStreamJs', '', true);
                    return;
                }

                if (payload.startsWith("data:")) {
                    const data = JSON.parse(payload.replace("data:", ""));
                    const content = data.choices[0].delta.content;
                    if (content) {
                        dotNetHelper.invokeMethodAsync('ChartCompletetionsStreamJs', content, false);
                    }
                }
            }
        }
    } catch (error) {
        // Handle fetch request errors
        console.log(error);
    } finally {
        // TODO: cleanup
    }
}

export async function createChatCompletions2(key, message, dotNetHelper) {
    const API_KEY = key;
    const API_URL = 'https://vk-aoai-test.openai.azure.com/openai/deployments/gpt-4o-mini/chat/completions?api-version=2024-02-15-preview';
    let messages = [];
    let notificationTriggered = false;
    let streamComplete = false;

    try {
        // Fetch the response from the OpenAI API with the signal from AbortController
        const response = await fetch(API_URL, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "api-key": `${API_KEY}`,
            },
            body: JSON.stringify({
                messages: [{
                    "role": "user",
                    "content": [{ "type": "text", "text": "What is Blazor?" }]
                }],
                "temperature": 0.7,
                "top_p":0.95,
                max_tokens: 200,
                stream: true, // For streaming responses
            })
        });

        // Read the response as a stream of data
        const reader = response.body.getReader();
        const decoder = new TextDecoder("utf-8");
        let i = 0;
        while (true) {
            const { done, value } = await reader.read();
            if (done) {
                break;
            }

            // Massage and parse the chunk of data
            const chunk = decoder.decode(value);
            const lines = chunk.split("\n");

            for (const payload of lines) {

                if (payload.includes('[DONE]')) {
                    streamComplete = true;
                    return;
                }

                if (payload.startsWith("data:")) {
                    const data = JSON.parse(payload.replace("data:", ""));
                    const content = data.choices[0]?.delta?.content;
                    if (content) {
                        messages.push(content);
                        if (!notificationTriggered) {
                            notificationTriggered = true;
                            triggerNotify();
                        }
                    }
                }
            }
        }

        function triggerNotify() {
            let handler = setInterval(() => {
                const content = messages.shift();
                dotNetHelper.invokeMethodAsync('ChartCompletetionsStreamJs', content, false);

                if (streamComplete && messages.length === 0) {
                    clearInterval(handler);
                    dotNetHelper.invokeMethodAsync('ChartCompletetionsStreamJs', '', true);
                }
            }, 100);
        }

    } catch (error) {
        // Handle fetch request errors
        console.log(error);
    } finally {
        // TODO: cleanup
    }
}