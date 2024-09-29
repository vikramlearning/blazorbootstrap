export async function createChatCompletions(key, messages, dotNetHelper) {
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
                messages: messages,
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
